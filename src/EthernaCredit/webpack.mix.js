const mix = require("laravel-mix")
require("laravel-mix-purgecss")

mix.disableNotifications()

// Standalone JS scripts
mix.js("Static/js/site.js", "js")
mix.js("Static/js/admin.js", "js")

// Styles
mix.sass("Static/scss/site.scss", "css", {}, [
  require("autoprefixer")
])
// .purgeCss({
//   // enabled: true,
//   content: [
//     "Areas/Admin/**/*.cshtml",
//     "Pages/**/*.cshtml",
//   ]
// })

// Vendor
mix.extract([
  "popper.js",
  "bootstrap",
  "jquery",
  "jquery-validation",
  "jquery-validation-unobtrusive",
  "jquery-datepicker"
])

// Options
mix.options({
  terser: {
    extractComments: false
  }
})
mix.setPublicPath("./wwwroot/dist")
mix.sourceMaps(false)