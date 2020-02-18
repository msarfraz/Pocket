define(['app', "Models/categoryModel"], function (app, Category) {

    function CategoryVM(values) {
        values = values || {};
        this.base = Category;
        this.base(values);

	    this.DisplayText = values['DisplayText'] || '';
	    this.Subcategories = values['Subcategories'];
		this.Show = this.Display == 1; //[{ value: 0, text: 'No', selected: false }, { value: 1, text: 'Yes', selected: false }];
	}

	

	return CategoryVM;
});