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
        var filter = currentSearch.toLowerCase();
        
        // Apply filter and search to fixture panels
        $('.fixture-panel').each(function() {
            var panel = $(this);
            var shouldShow = false;

            // Check if panel passes the filter
            if (currentFilter === 'all' ||
                (currentFilter === 'failed' && panel.hasClass('failed-fixture')) ||
                (currentFilter === 'success' && panel.hasClass('success-fixture'))) {
                
                // If panel passes the filter, check if it matches the search
                var fixtureName = panel.find('.fixture-name').text().toLowerCase();
                shouldShow = fixtureName.indexOf(filter) > -1;
                
                if (!shouldShow) {
                    panel.find('.searchable-content').each(function() {
                        if ($(this).text().toLowerCase().indexOf(filter) > -1) {
                            shouldShow = true;
                            return false; // break the loop
                        }
                    });
                }
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
                var testCaseName = panel.find('.panel-title').text().toLowerCase();
                shouldShow = testCaseName.indexOf(filter) > -1;
                
                if (!shouldShow) {
                    panel.find('.searchable-content').each(function() {
                        if ($(this).text().toLowerCase().indexOf(filter) > -1) {
                            shouldShow = true;
                            return false; // break the loop
                        }
                    });
                }
            }
            
            panel.toggle(shouldShow);
        });
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
});