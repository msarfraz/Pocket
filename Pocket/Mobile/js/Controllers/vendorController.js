define(["app", "Views/Vendor/vendorView", "Views/Vendor/vendorlistView", "Models/vendorModel", "pocketdataaccess"], function (app, VendorView, VendorListView, Vendor, Repo) {
    
	var state = {isNew: false};
	var vendor = null;
	var $$ = Framework7.$;

	var bindings = [{
		element: '.vendor-save-link',
		event: 'click',
		handler: saveVendor
	}];
	var listbindings = [{
	    element: '.vendor-delete',
	    event: 'click',
	    handler: deleteVendor
	}, {
	    element: '.vendor-edit-link',
	    event: 'click',
	    handler: editVendor
	}];

	function init(query){
		if (query && query.id) {
		    var vendors = Repo.GetVendors(); // JSON.parse(localStorage.getItem("f7Base"));
			for (var i = 0; i< vendors.length; i++) {
				if (vendors[i].VendorID == query.id) {
				    vendor = new Vendor(vendors[i]);
					state.isNew = false;
					break;
				}
			}
		}
		else {
		    vendor = new Vendor();
			state.isNew = true;
		}
		VendorView.render({
		    model: vendor,
			state: state,
			bindings: bindings
		});
	}
	function initlist(query) {
	    var vendors = Repo.GetVendors();
	    VendorListView.render({ model: vendors, bindings: listbindings });
	}

	function deleteVendor(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { VendorID: id };
	    var classid = ".vendor-swipeout-" + id;
	    
	    Repo.DeleteVendor(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function editVendor(e, t) {
	    var url = $$(e.currentTarget).data('url');

	    app.mainView.router.loadPage(url);
	    
	}
	function saveVendor() {
	   // app.Repository.LoadLists();
	    var formInput = app.f7.formToJSON('#vendoredit');

	    vendor.setValues(formInput);
	    if (!vendor.validate()) {
	        app.f7.alert("Please fill all mandatory fields.");
			return;
		}
	    app.Repository.SaveVendor(vendor, function (result) {
	        if (result.success) {
	            app.ReloadPreviousPage();
	        }
	        else
	            app.f7.alert(result.message);
		    
		});
		
	}

	return {
	    init: init,
        initlist:initlist
	};
});