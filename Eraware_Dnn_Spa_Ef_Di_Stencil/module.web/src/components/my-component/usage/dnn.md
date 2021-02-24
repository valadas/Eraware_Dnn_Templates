In Dnn SPA module content (when the view is an .html file), you can use some tokens to inject the module id. Learn more about those tokens at [DnnDocs](https://dnndocs.com/content/tutorials/modules/about-modules/spa-module-development/index.html#accessing-dnn-features).

In this example we use `[[AntiForgeryToken:True]]` to secure our api calls and ensure they only come from our page. We also use `[ModuleContext:ModuleId]` in order to inject our module id so we can call the proper apis and have a module context in the backend.

```html
[AntiForgeryToken:True]
<script type="module" src="DesktopModules/Contacts/resources/scripts/acm-contacts/acm-contacts.esm.js"></script>
<script nomodule="" src="DesktopModules/Contacts/resources/scripts/acm-contacts/acm-contacts.js"></script>
<my-component module-id="[ModuleContext:ModuleId]" />
```
