define(["Views/listView", "Models/contactModel", "app"], function (ListView, Contact, App) {

	var bindings = [{
		element: '.swipeout',
		event: 'deleted',
		handler: itemDeleted
	}];

	function init(query) {
	    var payees = App.Repository.GetRepository()._payees;
	    var contacts = [
			new Contact({ PayeeID: payees[0].PayeeID, Name: payees[0].Name }),
			new Contact({ PayeeID: payees[1].PayeeID, Name: payees[1].Name })
	    ];
		ListView.render({ model: payees, bindings: bindings });
	}

	function loadContacts() {
	    var f7Base ;//= localStorage.getItem("f7Base");
		var contacts = f7Base ? JSON.parse(f7Base) : tempInitializeStorage();
		return contacts;
	}

	function tempInitializeStorage() {
		var contacts = [
			new Contact({id: "1", firstName: "Alex", lastName: "Black", phone: "+380501234567" }),
			new Contact({id: "2", firstName: "Kate", lastName: "White", phone: "+380507654321" })
		];
		localStorage.setItem("f7Base", JSON.stringify(contacts));
		return JSON.parse(localStorage.getItem("f7Base"));
	}

	function itemDeleted(e) {
		var id = e.srcElement.id;
		var contacts = JSON.parse(localStorage.getItem("f7Base"));
		for (var i = 0; i < contacts.length; i++) {
			if (contacts[i].id === id) {
				contacts.splice(i, 1);
			}
		}
		localStorage.setItem("f7Base", JSON.stringify(contacts));
	}

	return {
		init: init
	};
});