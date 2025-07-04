var map;

function initialize() {
    var latlng = new google.maps.LatLng(32.42, -99.75);
    var options = {
        zoom: 14, center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    map = new google.maps.Map(document.getElementById("google-map"), options);

    var input = (document.getElementById('venue-input'));

    var searchBox = new google.maps.places.SearchBox((input));

    google.maps.event.addListener(searchBox, 'places_changed', function () {
        var places = searchBox.getPlaces();

        if (places.length == 0) {
            return;
        }

        // For each place, get the icon, place name, and location.
        markers = [];
        var bounds = new google.maps.LatLngBounds();
        for (var i = 0, place; place = places[i]; i++) {
            var image = {
                url: place.icon,
                size: new google.maps.Size(71, 71),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(17, 34),
                scaledSize: new google.maps.Size(25, 25)
            };

            // Create a marker for each place.
            var mark = new google.maps.Marker({
                map: map,
                icon: image,
                title: place.formatted_address,//place.name,
                position: place.geometry.location
            });

            markers.push(mark);

            bounds.extend(place.geometry.location);
        }

        map.fitBounds(bounds);
    });
}