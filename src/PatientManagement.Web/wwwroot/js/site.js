// Shared screen behaviour: confirmation for changes that cannot be undone, and no double submits.
// No business rules live here. The server still validates and decides everything.
(function () {
    'use strict';

    var modalElement = document.getElementById('confirmModal');
    var modalMessage = document.getElementById('confirmModalMessage');
    var modalConfirm = document.getElementById('confirmModalOk');
    var modal = (modalElement && window.bootstrap) ? new bootstrap.Modal(modalElement) : null;
    var pendingForm = null;
    var pendingSubmitter = null;

    // If a submit has not left the page after this long (for example a download, or a cancelled navigation),
    // the buttons are given back so the user is not stuck on a disabled form.
    var BUSY_RESET_MS = 15000;

    // A form with data-confirm is sent only after the user chooses Confirm in the dialog.
    // The original button is passed back, so its name and value still reach the server.
    if (modalConfirm) {
        modalConfirm.addEventListener('click', function () {
            var form = pendingForm;
            var submitter = pendingSubmitter;
            pendingForm = null;
            pendingSubmitter = null;
            if (modal) {
                modal.hide();
            }
            if (form) {
                form.dataset.confirmed = 'true';
                form.requestSubmit(submitter || undefined);
            }
        });
    }

    if (modalElement) {
        modalElement.addEventListener('hidden.bs.modal', function () {
            pendingForm = null;
            pendingSubmitter = null;
        });
    }

    // Disables the form's buttons and shows a spinner on the clicked one, so a second click does nothing.
    function markBusy(form, submitter) {
        // Each submit has its own token, so a timer from an earlier submit cannot reset a later one.
        var token = String(Date.now());
        form.dataset.submitting = token;

        // Wait one tick. A disabled button is left out of the form data, and the clicked button's value
        // (for example ConfirmWarnings) must still be read by the browser before it is disabled.
        window.setTimeout(function () {
            if (form.dataset.submitting !== token) {
                return;
            }

            Array.prototype.forEach.call(form.querySelectorAll('button'), function (button) {
                if ((button.getAttribute('type') || 'submit') === 'submit') {
                    button.disabled = true;
                    button.setAttribute('aria-busy', 'true');
                }
            });

            if (submitter && submitter.tagName === 'BUTTON') {
                var spinner = document.createElement('span');
                spinner.className = 'spinner-border spinner-border-sm me-2';
                spinner.setAttribute('aria-hidden', 'true');
                submitter.prepend(spinner);
            }

            window.setTimeout(function () {
                if (form.dataset.submitting === token) {
                    restoreForm(form);
                }
            }, BUSY_RESET_MS);
        }, 0);
    }

    // Puts a form's buttons back and removes the spinner.
    function restoreForm(form) {
        delete form.dataset.submitting;
        Array.prototype.forEach.call(form.querySelectorAll('button[aria-busy]'), function (button) {
            button.disabled = false;
            button.removeAttribute('aria-busy');
        });
        Array.prototype.forEach.call(form.querySelectorAll('.spinner-border'), function (spinner) {
            spinner.remove();
        });
    }

    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement) || event.defaultPrevented) {
            return;
        }

        if (form.dataset.submitting) {
            event.preventDefault();
            return;
        }

        // The confirmation flag is used once and cleared on every submit, so a failed submit cannot leave it set.
        var confirmed = form.dataset.confirmed === 'true';
        form.dataset.confirmed = '';

        // Invalid fields are handled by the browser and by jQuery validation. Nothing is disabled for them.
        if (!form.checkValidity()) {
            return;
        }

        var question = form.getAttribute('data-confirm');
        if (question && !confirmed) {
            if (modal && modalMessage) {
                event.preventDefault();
                pendingForm = form;
                pendingSubmitter = event.submitter || null;
                modalMessage.textContent = question;
                modal.show();
                return;
            }
            // Without Bootstrap's script, ask with the browser's own dialog and carry on with this same submit.
            if (!window.confirm(question)) {
                event.preventDefault();
                return;
            }
        }

        if (form.hasAttribute('data-no-busy')) {
            return;
        }

        markBusy(form, event.submitter);
    });

    // Lists marked data-table get paging, search and column sorting (approved 2026-11-12).
    // Default 10 rows; page sizes 10, 25, 50 and 100. The server's order is kept until a column is sorted.
    // Columns marked no-sort (for example Actions) cannot be sorted.
    if (window.jQuery && jQuery.fn.dataTable) {
        jQuery('table.data-table').each(function () {
            jQuery(this).DataTable({
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                order: [],
                columnDefs: [{ orderable: false, targets: 'no-sort' }]
            });
        });
    }

    // Going back to a page can restore it with its buttons still disabled. Put them back.
    window.addEventListener('pageshow', function () {
        Array.prototype.forEach.call(document.querySelectorAll('form[data-submitting]'), restoreForm);
    });
})();
