// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

mermaid.init({ htmlLabels: true, securityLevel: "antiscript" }, $("code.lang-mermaid"));

document.addEventListener("DOMContentLoaded", () => {
    const redoc = document.querySelector("redoc");

    if (redoc) {
        const mainContent = document.querySelector(".main-content");
        mainContent.innerHTML = "";
        mainContent.appendChild(redoc);
    }
});
