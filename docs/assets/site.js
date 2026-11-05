/* Slime Chronicles site script. No build step, no dependencies besides marked (doc.html only). */
(function () {
  "use strict";

  var REPO = "Oracle451/IMG-420-Final-Project";

  /* ------------------------------------------------------------------
   * Landing page: show which commit the rolling release was built from.
   * Everything degrades gracefully; the static HTML works without this.
   * ------------------------------------------------------------------ */
  function initBuildInfo() {
    var info = document.getElementById("build-info");
    if (!info) return;

    fetch("https://api.github.com/repos/" + REPO + "/releases/tags/rolling", {
      headers: { Accept: "application/vnd.github+json" },
    })
      .then(function (res) {
        if (res.status === 404) throw new Error("none");
        if (!res.ok) throw new Error("http " + res.status);
        return res.json();
      })
      .then(function (rel) {
        var match = /\(([0-9a-f]{7,40})\)/.exec(rel.name || "");
        var when = rel.published_at ? new Date(rel.published_at) : null;

        info.textContent = "Latest build";
        if (match) {
          info.appendChild(document.createTextNode(" from commit "));
          var link = document.createElement("a");
          link.href = "https://github.com/" + REPO + "/commit/" + match[1];
          var code = document.createElement("code");
          code.textContent = match[1];
          link.appendChild(code);
          info.appendChild(link);
        }
        if (when && !isNaN(when)) {
          info.appendChild(
            document.createTextNode(
              " · " + when.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" })
            )
          );
        }

        (rel.assets || []).forEach(function (asset) {
          document.querySelectorAll('[data-size="' + asset.name + '"]').forEach(function (el) {
            el.textContent = "· " + (asset.size / 1048576).toFixed(1) + " MB";
          });
        });
      })
      .catch(function (err) {
        if (err && err.message === "none") {
          info.textContent = "The first rolling build hasn’t been published yet — check back soon!";
        }
        /* any other error: keep the default text */
      });
  }

  /* ------------------------------------------------------------------
   * doc.html: render the repo's Markdown docs in the page.
   * Only the whitelisted files below can be loaded.
   * ------------------------------------------------------------------ */
  var DOCS = {
    guide: { title: "User Guide", file: "USER_GUIDE.md" },
    api: { title: "API Reference", file: "API_REFERENCE.md" },
    build: { title: "Build Instructions", file: "BUILD_INSTRUCTIONS.md" },
  };

  function slugify(text) {
    return text
      .toLowerCase()
      .replace(/[^a-z0-9\s-]/g, "")
      .trim()
      .replace(/\s+/g, "-");
  }

  function initDocViewer() {
    var target = document.getElementById("doc");
    var nav = document.getElementById("doc-nav");
    if (!target || !nav) return;

    var key = new URLSearchParams(location.search).get("p");
    if (!Object.prototype.hasOwnProperty.call(DOCS, key)) key = "guide";
    var doc = DOCS[key];

    Object.keys(DOCS).forEach(function (k) {
      var li = document.createElement("li");
      var a = document.createElement("a");
      a.href = "doc.html?p=" + k;
      a.textContent = DOCS[k].title;
      if (k === key) a.setAttribute("aria-current", "page");
      li.appendChild(a);
      nav.appendChild(li);
    });
    var pdf = document.createElement("li");
    var pdfLink = document.createElement("a");
    pdfLink.href = "DESIGN_DOCUMENT.pdf";
    pdfLink.textContent = "Design Document (PDF)";
    pdf.appendChild(pdfLink);
    nav.appendChild(pdf);

    document.title = doc.title + " — Slime Chronicles";

    fetch(doc.file)
      .then(function (res) {
        if (!res.ok) throw new Error("http " + res.status);
        return res.text();
      })
      .then(function (md) {
        target.innerHTML = window.marked.parse(md, { gfm: true });

        /* anchors for headings */
        target.querySelectorAll("h1, h2, h3, h4").forEach(function (h) {
          if (!h.id) h.id = slugify(h.textContent);
        });

        /* links between the docs stay inside the viewer */
        target.querySelectorAll("a[href]").forEach(function (a) {
          var href = a.getAttribute("href");
          Object.keys(DOCS).forEach(function (k) {
            if (href === DOCS[k].file || href === "./" + DOCS[k].file) a.setAttribute("href", "doc.html?p=" + k);
          });
          if (/^https?:/i.test(href)) a.rel = "noopener";
        });

        var h1 = target.querySelector("h1");
        if (h1) document.title = h1.textContent.replace(/\*/g, "") + " — Slime Chronicles";

        var meta = document.createElement("p");
        meta.className = "doc-meta";
        var edit = document.createElement("a");
        edit.href = "https://github.com/" + REPO + "/blob/main/docs/" + doc.file;
        edit.textContent = "Edit this page on GitHub";
        meta.appendChild(edit);
        target.appendChild(meta);

        if (location.hash) {
          var el = document.getElementById(decodeURIComponent(location.hash.slice(1)));
          if (el) el.scrollIntoView();
        }
      })
      .catch(function () {
        target.innerHTML = "";
        var err = document.createElement("p");
        err.className = "doc-error";
        err.append("Couldn’t load this document. You can read it on ");
        var a = document.createElement("a");
        a.href = "https://github.com/" + REPO + "/blob/main/docs/" + doc.file;
        a.textContent = "GitHub";
        err.appendChild(a);
        err.append(" instead. (Opening this page straight from disk? Browsers block that — serve the folder with any local web server.)");
        target.appendChild(err);
      });
  }

  initBuildInfo();
  initDocViewer();
})();
