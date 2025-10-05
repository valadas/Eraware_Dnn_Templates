Simply load the javascript library, provide it a module id and you are good to go.

```html
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Document</title>
       <script type="module" src="some-path/acm-contacts.esm.js"></script>
       <script nomodule="" src="some-path/acm-contacts.js"></script>
    </head>
    <body>
        <my-component module-id="123" />        
    </body>
    </html>
```
