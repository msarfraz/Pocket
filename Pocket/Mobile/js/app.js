require.config({
	paths: {
		handlebars: "../lib/handlebars",
		text: "../lib/text",
		hbs: "../lib/hbs",
	//	framework7: "../lib/framework7",
		lodash: "../lib/lodash.min"
	},
	shim: {
		handlebars: {
			exports: "Handlebars"
		}
	}
});
define('app', ['router', 'pocketdataaccess'], function (Router, Repository) {

    

	var f7 = new Framework7({
		modalTitle: 'Xpert Budget',
		animateNavBackIcon: true,
		swipeBackPage: false,
	    uniqueHistory: true,
	    swipePanel: 'left',
	    precompileTemplates: true,
	    swipePanelActiveArea: 40,

	    material: true,

	    // for performance
	    preloadPreviousPage: false,
	    animatePages: false,
	    sortable: false,
	    swipeBackPageAnimateShadow: false,
	    swipeBackPageAnimateOpacity: false,
	   // swipeout: false,
	    swipeoutNoFollow: true,
	    swipePanelNoFollow: true
	});

	

	var mainView = f7.addView('.view-main', {
	    //dynamicNavbar: true,
	    smartSelectBackOnSelect: true
	    //domCache: true,
	    
	});
	var $$ = Dom7;
    /**/
	Template7.global = {
	    IsLocal: true,
	    LocalClass: 'disabled'
	};

	$$('.login-local').on('click', function () {
	    localStorage.setItem('OFFLINE', true);
	    LoginLocal();

	});
	$$('.login-link').on('click', function () {
	    var data = { UserName: $$('.login-name').val(), Password: $$('.login-password').val(), RememberMe: $$('.login-rememberme').is(':checked') };
	    if (data.UserName == '' || data.Password == '')
	    {
	        f7.alert("Please provide Login name and Password.");
	        return;
	    }

	    Repository.Login(data, LoginCallback);
	    
	});
	$$('.register-link').on('click', function () {
	    var data = { UserName: $$('.login-name').val(), Password: $$('.login-password').val(), ConfirmPassword: $$('.login-confirm-password').val(), Email: $$('.login-email').val() };
	    if (data.UserName == '' || data.Password == '' || data.ConfirmPassword == '' || data.Email == '') {
	        f7.alert("Please fill all the fields.");
	        return;
	    }

	    Repository.Register(data, function (response) {

	        f7.hideIndicator();
	        if (response.rows[0].success)
	        {
	            f7.alert(response.rows[0].message, function () {
	                LoginCallback(response);
	            });
	            
	        }
	        else
	        {
	            f7.alert(response.rows[0].message);
	        }
	        
	    });

	});
	$$('.logout-link').on('click', function () {

	    Repository.Logout();
	    localStorage.clear();
	    ResetLoginForm();
	    ShowLogin();
	    f7.loginScreen();
	});
	function LoginCallback(response)
	{
	    if (response.rows) {
	        var result = response.rows[0];
	        if (result.success) {
	            f7.closeModal();
	            $$(".login-auto").hide();
	            mainView.router.loadPage("Pages/Home.html");
	            mainView.router.refreshPage();
	            Repository.GetNotificationsCount({}, function (ncount) {
	                if (ncount.new_id && ncount.new_id > 0)
	                {
	                    $$(".notification-count").html(ncount.new_id);
	                    $$(".notification-count").addClass('badge');
	                }
	                
	            });
	        }
	        else {
	            ShowLogin();
	            $$(".login-auto").hide();
                if(result.message)
	                f7.alert(result.message);
	        }
	    }
	    else {
	        f7.alert("Invalid user name or password");
	    }
	    f7.hideIndicator();
	}
	function LoginLocal()
	{
	    console.log("LoginLocal");
	    f7.closeModal();
	    $$(".login-auto").hide();
	    $$(".pro-link").hide();
	    Router.init(f7);
	    Repository.initialize(f7, true, function () {
	        console.log("Repository.initialize");
	    mainView.router.loadPage("Pages/Home.html");
	   
	    });
	    
	}
	function ReloadPreviousPage()
	{
	    mainView.router.loadPage(mainView.history[mainView.history.length - 2]);
	}
	function ShowWelcomeScreen()
	{
	    if (localStorage.getItem('Welcomed'))
	        return;
	    
	    var welcomescreen_slides = [
                  {
                      id: 'slide0',
                      picture: '<div class="tutorialicon"><img src="Images/Chart2.png" width="300" height="170" /></div>',
                      text: 'Welcome to this tutorial. In the next steps we will guide you how to use this app.'
                  },
                  {
                      id: 'slide1',
                      picture: '<div class="tutorialicon"><img src="Images/Budget.png" width="300" height="170"/></div>',
                      text: 'Define your budget'
                  },
                  {
                      id: 'slide2',
                      picture: '<div class="tutorialicon"><img src="Images/Account.png" width="300" height="170"/></div>',
                      text: 'Insert your transactions'
                  },
                  {
                      id: 'slide3',
                      picture: '<div class="tutorialicon"><img src="Images/Targets.png" width="300" height="170"/></div>',
                      text: 'Track your targets'
                  },
                  {
                      id: 'slide4',
                      picture: '<div class="tutorialicon"><img src="Images/Events.png" width="300" height="170"/></div>',
                      text: 'Group your expenses w.r.t events'
                  },
                  {
                      id: 'slide5',
                      picture: '<div class="tutorialicon"><img src="Images/Calendar.png" width="300" height="170"/></div>',
                      text: 'Monitor your calendar'
                  },
                  {
                      id: 'slide6',
                      picture: '<div class="tutorialicon"><img src="Images/Chart.png" width="300" height="170"/></div>',
                      text: 'See the data in graphical format'
                  },
	                {
	                    id: 'slide7',
	                    picture: '<div class="tutorialicon"></div>',
	                    text: 'Thanks for reading! Enjoy this app.<br><br><a id="tutorial-close-btn" class="button button-fill button-raised color-red" href="#">End Tutorial</a>'
	                }
	    ];
	    var options = {
	        'bgcolor': '#7676BC',
	        'fontcolor': '#fff',
	        closeButtonText : 'Xpert Budget!'
	    }
	    var welcomescreen = f7.welcomescreen(welcomescreen_slides, options);
	    $$('#tutorial-close-btn').on('click', function () {
	        welcomescreen.close();
	    });

	    localStorage.setItem('Welcomed', 1);
	}
	function AutoLogin()
	{
	    console.log("AutoLogin");
	    $$('.statusbar-overlay').addClass('bg-purple');
        ShowWelcomeScreen();
	    LoginLocal();
	    return;

	    $$(".login-connection").hide();
	    if (localStorage.getItem('OFFLINE'))
	    {
	        LoginLocal();
	    }
	    else if (localStorage.getItem('AT')) {
	        $$(".login-auto").show();
	        Repository.AutoLogin({}, function (response) {
	            
	            LoginCallback(response);
	            
	        });
	    }
	    else
	    {
	        ShowLogin();
	    }
	    
	}
	function ResetLoginForm() {
	    $$('.login-name').val('');
	    $$('.login-password').val('');
	    $$('.login-confirm-password').val('');
	    $$('.login-email').val('');
	    $$('.login-rememberme').attr('checked', false);;
	    $$(".login-auto").hide();
	}
	function ShowLogin() {
	    
	    $$(".register-data").hide();
	    $$(".login-data").show();
	}
	function ShowRegister() {
	    ResetLoginForm();
	    $$(".register-data").show();
	    $$(".login-data").hide();
	}
	return {
		f7: f7,
		mainView: mainView,
		router: Router,
		Repository: Repository,
		AutoLogin: AutoLogin,
		ReloadPreviousPage: ReloadPreviousPage
	};
});

