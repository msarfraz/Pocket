/**
 * A plugin for Framework7 to give the user a button for scrolling up
 *
 * @author www.timo-ernst.net
 * @license MIT
 */
Framework7.prototype.plugins.upscroller = function (app, globalPluginParams) {
  'use strict';
  
  var Upscroller = function (text) {
      var self = this,
        $$ = Dom7,
        $ = Framework7.$;
      var curpage;
      curpage = $('.page-content')[$('.page-content').length-1];
    
    var $$pages = $$('.page-content');
    var $$btn = $$('<div class="upscroller">↑ ' + text + '</div>');
    var $$body = $$('body');
    $$body.prepend($$btn);

    $$btn.click(function(event){
      event.stopPropagation();
      event.preventDefault();
      curpage.scrollTop = 0; //(0, Math.round(curpage.scrollTop() / 4))
      //$(curpage).animate({scrollTop:0}, Math.round(curpage.scrollTop/4));
    });

    $$pages.scroll(function(event){
        var page = event.target;
        curpage = page;
      if (page.scrollTop > 300) {
        $$btn.addClass('show');
      }
      else {
        $$btn.removeClass('show');
      }
    });
    
    $$pages.each(function (i, page) {
      var pagename = $$(page).parent().attr('data-page');
      app.onPageBeforeAnimation(pagename, function (thepage) {
          if ($(thepage.container).children('.page-content').length > 0)
          {
              curpage = $(thepage.container).children('.page-content')[0];
              var scrollpos = curpage.scrollTop;
              if (scrollpos > 300) {
                  $$btn.addClass('show');
              }
              else {
                  $$btn.removeClass('show');
              }
          }
        
      });
    });
    
    return this;
  };
  
  app.upscroller = function (text) {
    return new Upscroller(text);
  };
  
};