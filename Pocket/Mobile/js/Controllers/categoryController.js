define(["app", "Views/Category/categoryView", "Views/Category/categorylistView", "Models/categoryModel", "ViewModels/categoryViewModel", "Views/Category/subcategoryView", "Models/subcategoryModel", "ViewModels/subcategoryViewModel", "pocketdataaccess"], function (app, CategoryView, CategoryListView, Category, CategoryVM, SubcategoryView,Subcategory, SubcategoryVM, Repo) {
    
    var state = { isNew: false };

	var category = null;
	var categoryvm = null;
	var subcategory = null;
	var subcategoryvm = null;

	var myApp = app.f7;
	var $$ = Framework7.$;

	var bindings = [{
		element: '.category-save-link',
		event: 'click',
		handler: saveCategory
	},
	{
	    element: '.subcategory-save-link',
	    event: 'click',
	    handler: saveSubcategory
	},
	,
	{
	    element: '.subcategory-delete-link',
	    event: 'click',
	    handler: deleteSubcategory
	}];

	var listBindings = [{
	    element: '.category-delete-link',
	    event: 'click',
	    handler: deleteCategory
	}];

	function init(query) {
	    if (query && query.id) {
	        Repo.GetCategoryByID(query.id, function (response) {
	            categoryvm = new CategoryVM(response.rows[0]);
	            state.isNew = false;
	            CategoryView.render({
	                model: categoryvm,
	                state: state,
	                bindings: bindings
	            });
                
	        });
		}
		else {
		    categoryvm = new CategoryVM();
		    state.isNew = true;
		    CategoryView.render({
		        model: categoryvm,
		        state: state,
		        bindings: bindings
		    });
		}

		
	}
	function scatinit(query) {
	    if (query && query.id && query.catid) {

	        subcategoryvm = new SubcategoryVM(Repo.GetSubcategoryByID(categoryvm, query.id));
	        state.isNew = false;
	    }
	    else {
	        subcategoryvm = new SubcategoryVM();
	        state.isNew = true;
	    }

	    subcategoryvm.CategoryID = query.catid;
	    subcategoryvm.RepeatTypes = Repo.GetRepeatTypes();

	    SubcategoryView.render({
	        model: subcategoryvm,
	        state: state,
	        bindings: bindings
	    });
	    $$(".subcat-budget-duration").val(subcategoryvm.BudgetDuration);
	    var kp = myApp.keypad({
	        input: '#SubcategoryBudget',
	        type: 'numpad',
	        toolbar: true
	    });
	}
	function initlist(query) {
	    Repo.GetCategories(function (response) {
	        CategoryListView.render({ model: response.rows, bindings: listBindings });
	        
	    });
	}
	function deleteCategory(e,t)
	{
	    var id = $$(e.currentTarget).data('id');
	    var data = { CategoryID: id };
	    var classid = ".category-swipeout-" + id;

	    Repo.DeleteCategory(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });

	}
	function deleteSubcategory(e,t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { SubcategoryID: id };
	    var classid = ".subcategory-swipeout-" + id;

	    Repo.DeleteSubcategory(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });

	    
	}

	function saveCategory() {
	   // app.Repository.LoadLists();
	    var formInput = app.f7.formToJSON('#categoryedit');
	    //alert("savecategory called." + formInput);
	    if (!category) {
	        category = new Category();
	    }
	    category.setValues(formInput);
	    if (!category.validate()) {
	        app.f7.alert("Please fill all mandatory fields");
			return;
		}
	    app.Repository.SaveCategory(category, function (result) {
	        if (result.success) {
	            if (!category.CategoryID)
	                init({ id: result.new_id });
	            else
	                app.mainView.router.reloadPage("Pages/CategoryList.html"); //app.ReloadPreviousPage();
	        }
	        else
	            app.f7.alert(result.message);
		});
		
	}
	function saveSubcategory() {
	    var formInput = app.f7.formToJSON('#subcategoryedit');
	    if (!subcategory)
	        subcategory = new Subcategory();

	    subcategory.setValues(formInput);
	    if (!subcategory.validate()) {
	        app.f7.alert("Please fill all mandatory fields");
	        return;
	    }
	    app.Repository.SaveSubcategory(subcategory, function (result) {
	        //app.router.load('category', 'init');
	        if (result.success) {
	            app.ReloadPreviousPage();

	        }
	        else
	            app.f7.alert(result.message);
	    });

	}
	return {
	    init: init,
	    initlist: initlist,
	    scatinit: scatinit
	};
});