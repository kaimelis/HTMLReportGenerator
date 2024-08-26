$(document).ready(function() {
    $('[data-toggle="tooltip"]').tooltip({'placement': 'bottom'});

    // Add filtering functionality
    $('.btn-group input[type="radio"]').change(function() {
        var selectedFilter = $(this).parent().text().trim().toLowerCase();
        if (selectedFilter === 'all') {
            $('.fixture-panel').show();
        } else if (selectedFilter === 'failed') {
            $('.fixture-panel').hide();
            $('.failed-fixture').show();
        } else if (selectedFilter === 'success') {
            $('.fixture-panel').hide();
            $('.success-fixture').show();
        }
    });

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
    });

    // Reset modal size on hide
    $('.modal').on('hidden.bs.modal', function() {
        var dialog = $(this).find('.modal-dialog');
        dialog.css({
            width: '',
            height: ''
        });
    });
    
    // Search bar functionality
    $('#searchBar').on('keyup', function() {
        var value = $(this).val().toLowerCase(); // Get the value of the search bar and convert it to lowercase
        $('.fixture-panel').filter(function() {
            // Toggle visibility of the fixture panels based on the search query
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
});