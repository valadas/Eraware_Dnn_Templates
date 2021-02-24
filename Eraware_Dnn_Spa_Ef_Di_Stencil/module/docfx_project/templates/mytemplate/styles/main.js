document.addEventListener("DOMContentLoaded", () => {
    const redoc = document.querySelector("#redoc");

    if (redoc) {
        const mainContent = document.querySelector(".main-content");
        mainContent.innerHTML = "";
        mainContent.appendChild(redoc);

        var redocScript = document.createElement("script");
        redocScript.type = "text/javascript";
        redocScript.src = "https://cdn.jsdelivr.net/npm/redoc@next/bundles/redoc.standalone.js";
        document.head.appendChild(redocScript);

        var timer = setInterval(function () {
            if (window.Redoc !== undefined) {
                clearInterval(timer);
                Redoc.init(
                    "rest.json",
                    {
                        scrollYOffset: 91,
                    },
                    redoc
                );
            }
        }, 200);

    }

});

mermaid.init({ startOnLoad: false, htmlLabels: true, securityLevel: "antiscript" }, $("code.lang-mermaid"));