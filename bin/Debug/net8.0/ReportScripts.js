$(document).ready(function() {
    $('[data-toggle="tooltip"]').tooltip({'placement': 'bottom'});

    var currentFilter = 'all';
    var currentSearch = '';

    // Add filtering functionality
    $('.btn-group input[type="radio"]').change(function() {
        currentFilter = $(this).parent().text().trim().toLowerCase();
        applyFilterAndSearch();
    });

    function applyFilterAndSearch() {
        // Apply filter and search to fixture panels
        $('.fixture-panel').each(function() {
            var panel = $(this);
            var shouldShow = false;

            // Check if panel passes the filter
            if (currentFilter === 'all' ||
                (currentFilter === 'failed' && panel.hasClass('failed-fixture')) ||
                (currentFilter === 'success' && panel.hasClass('success-fixture'))) {
                
                // If panel passes the filter, check if it matches the search
                shouldShow = searchForRecipePath(panel);
            }
            
            panel.toggle(shouldShow);
        });
        
        // Apply filter and search to modal test case panels
        $('.test-case-panel').each(function() {
            var panel = $(this);
            var shouldShow = false;

            // Check if panel passes the filter
            if (currentFilter === 'all' ||
                (currentFilter === 'failed' && (panel.data('test-result') === 'failure' || panel.data('test-result') === 'error')) ||
                (currentFilter === 'success' && panel.data('test-result') === 'success')) {
                
                // If panel passes the filter, check if it matches the search
                shouldShow = searchForRecipePath(panel);
            }
            
            panel.toggle(shouldShow);
        });
    }

    function searchForRecipePath(panel) {
        if (currentSearch === '') return true;

        var generalSection = panel.find('.panel-title:contains("General")').closest('.panel-default');
        if (generalSection.length === 0) return false;

        var found = false;
        generalSection.find('table').each(function() {
            var table = $(this);
            var recipePathRow = table.find('tr').filter(function() {
                return $(this).find('td:first').text().trim() === 'Recipe Path';
            });

            if (recipePathRow.length > 0) {
                var recipePathValue = recipePathRow.find('td:nth-child(2)').text().toLowerCase();
                if (recipePathValue.indexOf(currentSearch.toLowerCase()) > -1) {
                    found = true;
                    return false; // Break the .each() loop
                }
            }
        });

        return found;
    }

    // Make modals resizable
    $('.modal-dialog').resizable({
        handles: 'se',
        minHeight: 300,
        minWidth: 300,
        start: function(event, ui) {
            // Store original dimensions
            $(this).data('originalWidth', $(this).width());
            $(this).data('originalHeight', $(this).height());
        },
        resize: function(event, ui) {
            var dialog = $(this);
            var originalWidth = dialog.data('originalWidth');
            var originalHeight = dialog.data('originalHeight');
            
            // Only update if size has increased
            if (ui.size.width > originalWidth) {
                dialog.css('width', ui.size.width + 'px');
            }
            if (ui.size.height > originalHeight) {
                dialog.css('height', ui.size.height + 'px');
            }
            
            // Adjust modal body height
            var content = dialog.find('.modal-content');
            var header = content.find('.modal-header');
            var body = content.find('.modal-body');
            var footer = content.find('.modal-footer');
            var newBodyHeight = content.height() - header.outerHeight() - footer.outerHeight();
            body.height(Math.max(newBodyHeight, 100)); // Ensure minimum height
        }
    });

    // Reposition modal on show
    $('.modal').on('show.bs.modal', function() {
        var dialog = $(this).find('.modal-dialog');
        dialog.css({
            width: 'auto',
            height: 'auto',
            'max-height': '80vh' // Limit initial height to 80% of viewport height
        });
        
        // Apply current filter and search to modal content
        applyFilterAndSearch();
    });

    // Reset modal size on hide
    $('.modal').on('hidden.bs.modal', function() {
        var dialog = $(this).find('.modal-dialog');
        dialog.css({
            width: '',
            height: ''
        });
    });

    // Add search functionality
    $('#searchBar').on('keyup', function() {
        currentSearch = this.value;
        applyFilterAndSearch();
    });

    // Initialize error list panel state
    function initializeErrorListPanel() {
        var $panel = $('#errorContainer');
        var $icon = $panel.find('.panel-heading i');
        var $content = $('#errorsList');

        if ($content.hasClass('in')) {
            $icon.removeClass('glyphicon-chevron-down').addClass('glyphicon-chevron-up');
        } else {
            $icon.removeClass('glyphicon-chevron-up').addClass('glyphicon-chevron-down');
        }
    }

    // Toggle error list panel
    $('#errorContainer .panel-heading').click(function() {
        var $icon = $(this).find('i');
        $icon.toggleClass('glyphicon-chevron-down glyphicon-chevron-up');
    });

    // Call the initialization function when the page loads
    initializeErrorListPanel();
});