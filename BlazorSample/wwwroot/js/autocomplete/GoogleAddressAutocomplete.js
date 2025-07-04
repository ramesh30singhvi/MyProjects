// This sample uses the Places Autocomplete widget to:
// 1. Help the user select a place
// 2. Retrieve the address components associated with that place
// 3. Populate the form fields with those address components.
// This sample requires the Places library, Maps JavaScript API.
// Include the libraries=places parameter when you first load the API.
// For example: <script
// src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=places">
let autocomplete;
let address1Field;
let address2Field;
let cityField;
let stateField;
let countryField;
let stateName;
let businessPhoneField;
let businessPhoneCountryField;
let companyNameField;
let geoLatitude;
let geoLongitude;

function initAutocomplete() {
    address1Field = document.querySelector("#inputAddress1");
    address2Field = document.querySelector("#inputAddress2");
    postalField = document.querySelector("#inputPostalCode");
    cityField = document.querySelector("#inputCity");
    stateField = document.querySelector("#selState");
    stateFieldOpt = document.getElementById("#selStateOption");
    countryField = document.querySelector("#selCountry");
    companyNameField = document.querySelector("#inputVendorCompany");
    businessPhoneField = document.querySelector("#inputBusinessPhone");
    businessPhoneCountryField = document.querySelector("#selBusinessPhoneCountry");
    // Create the autocomplete object, restricting the search predictions to
    // addresses in the US and Canada.
    autocomplete = new google.maps.places.Autocomplete(address1Field, {
        componentRestrictions: { country: ["US"] },
        fields: ["address_components", "geometry", "formatted_phone_number", "name"],
    });

    address1Field.focus();
    // When the user selects an address from the drop-down, populate the
    // address fields in the form.
    autocomplete.addListener("place_changed", fillInAddress);
    var address = {
        "Address1": address1Field?.value ?? "",
        "ZipCode": postalField?.value ?? "",
        "City": cityField?.value ?? "",
        "State": stateName ?? "",
        "Country": countryField?.value ?? "",
        "BusinessPhone": businessPhoneField ? businessPhoneField.value : "",
        "BusinessPhoneCountry": businessPhoneCountryField ? businessPhoneCountryField.value : "",
        "CompanyName": companyNameField ? companyNameField.value : "",
        "GeoLatitude": String(geoLatitude ?? ""),
        "GeoLongitude": String(geoLongitude ?? "")
    };
    return address;
}

//Hide suggest address field 
function HideDisplaySuggestAddressField(isDisplay) {
    var addressSuggestOtions = document.getElementsByClassName("pac-container");
    if (addressSuggestOtions) {
        for (var i = 0; i < addressSuggestOtions.length; i++) {
            if (isDisplay) {
                addressSuggestOtions[i].classList.add("d-none");
            } else {
                addressSuggestOtions[i].classList.remove("d-none");
            }
        }
    }
    
    return true;
}
function fillInAddress() {
    // Get the place details from the autocomplete object.
    const place = autocomplete.getPlace();
    let address1 = "";
    let postcode = "";

    if (place != null)
    {
        if (place.formatted_phone_number && businessPhoneField) {
            businessPhoneField.value = place.formatted_phone_number;
        }
    }
    //if (place.name && companyNameField) {
    //    companyNameField.value = place.name;
    //}
    if (place.geometry) {
        geoLatitude = place.geometry.location.lat();
        geoLongitude = place.geometry.location.lng();
    }

    // Get each component of the address from the place details,
    // and then fill-in the corresponding field on the form.
    // place.address_components are google.maps.GeocoderAddressComponent objects
    // which are documented at http://goo.gle/3l5i5Mr
    for (const component of place.address_components) {
        const componentType = component.types[0];

        switch (componentType) {
            case "street_number": {
                address1 = `${component.long_name} ${address1}`;
                break;
            }

            case "route": {
                address1 += component.short_name;
                break;
            }

            case "postal_code": {
                postcode = `${component.long_name}${postcode}`;
                break;
            }

            case "postal_code_suffix": {
                postcode = `${postcode}-${component.long_name}`;
                break;
            }
            case "locality":
                if (cityField)
                    cityField.value = component?.long_name;
                break;

            case "administrative_area_level_1": {
                //if (stateField)
                //    stateField.value = component.short_name;
                if (stateFieldOpt) {
                    stateFieldOpt.value = component.short_name;
                    component.short_name.textContent = component.large_name;
                }
                stateName = component.short_name;
                break;
            }
            case "country":
                if (countryField)
                    countryField.value = component.short_name;
                if (businessPhoneCountryField) {
                    businessPhoneCountryField.value = component.short_name;
                }
                break;
        }
    }



    address1Field.value = address1;
    if (postalField)
        postalField.value = postcode;
    // After filling the form with address components from the Autocomplete
    // prediction, set cursor focus on the second address line to encourage
    // entry of subpremise information such as apartment, unit, or floor number.
    address1Field.focus();
    if (address2Field) {
        address2Field.value = "";
        address2Field.focus();
    }
}



let venue;
let latitude;
let longitude;
let venueField;
function addressAutocomplete() {

    venueField = document.querySelector("#txtAutocomplete");
    var autocomplete = new google.maps.places.Autocomplete(document.getElementById('txtAutocomplete'), {
        componentRestrictions: { country: ["US"] }
    });
    venueField.focus();
    address1Field = document.querySelector("#inputAddress1");
    google.maps.event.addListener(autocomplete, 'place_changed', function () {
        // Get the place details from the autocomplete object.
        var place = autocomplete.getPlace();
        venue = place.formatted_address;
        latitude = place.geometry.location.lat();
        longitude = place.geometry.location.lng();
        venueField.value = venue;
        venueField.focus();
    });
    var addressInfo = {
        "Address1": venue ?? "",
        "GeoLatitude": String(latitude ?? ""),
        "GeoLongitude": String(longitude ?? "")
    };
    //venueField.focus();
    return addressInfo;
}
