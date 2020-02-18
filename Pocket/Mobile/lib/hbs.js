define(["handlebars"], function (Handlebars) {
    Handlebars = Handlebars || this.Handlebars;
    Handlebars.registerHelper("js_compare", Template7.helpers["js_compare"]);
    Handlebars.registerHelper("to_date", function (strdate, options) {
        var d = strdate.split('-');
        
        if (d.length > 0) {
            return d[2] + "-" + months[d[1]-1] + "-" + d[0];
        }
        else
            return strdate;
    });

    Handlebars.registerHelper("global", function (options) {
        console.log("global=" + Template7.global);
        return Template7.global;
    });

  var templateExtension = ".hbs";
  var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

  return {

    pluginBuilder: "./hbs-builder",

    // http://requirejs.org/docs/plugins.html#apiload
    load: function (name, parentRequire, onload, config) {

      // Get the template extension.
      var ext = (config.hbs && config.hbs.templateExtension ? config.hbs.templateExtension : templateExtension);

      // In browsers use the text-plugin to the load template. This way we
      // don't have to deal with ajax stuff
      parentRequire(["text!" + name + ext], function (raw) {
        // Just return the compiled template
        onload(Handlebars.compile(raw));
      });

    }

  };
});
