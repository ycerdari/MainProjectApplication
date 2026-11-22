const fs = require("fs");
const zlib = require("zlib");
const path = require("path");

function unpack(html) {
  const m = html.match(/<script type="__bundler\/manifest">\s*([\s\S]*?)\s*<\/script>/);
  if (!m) return null;
  const manifest = JSON.parse(m[1]);
  const orderMatch = html.match(/<script type="__bundler\/page_order">\s*([\s\S]*?)\s*<\/script>/);
  const order = orderMatch ? JSON.parse(orderMatch[1]) : Object.keys(manifest);
  const templateMatch = html.match(/<script type="__bundler\/template">\s*([\s\S]*?)\s*<\/script>/);
  let titles = [];
  if (templateMatch) {
    const template = JSON.parse(templateMatch[1]);
    titles = [...template.matchAll(/<h2>([^<]+)/g)].map((x) => x[1]);
  }
  return order.map((id, i) => {
    const entry = manifest[id];
    const buf = Buffer.from(entry.data, "base64");
    const decoded = entry.compressed ? zlib.gunzipSync(buf).toString("utf8") : buf.toString("utf8");
    return { id, title: titles[i] || id, html: decoded, mime: entry.mime };
  });
}

const root = fs.readFileSync("c:/Users/Admin/Downloads/PUPlay UI Mockups (2).html", "utf8");
const boards = unpack(root);
const outDir = path.join(__dirname, "_mockup-extract");
fs.mkdirSync(outDir, { recursive: true });

function visibleText(html) {
  return html
    .replace(/<script[\s\S]*?<\/script>/gi, " ")
    .replace(/<style[\s\S]*?<\/style>/gi, " ")
    .replace(/<[^>]+>/g, "\n")
    .replace(/&amp;/g, "&")
    .replace(/&#39;/g, "'")
    .replace(/&quot;/g, '"')
    .split("\n")
    .map((s) => s.replace(/\s+/g, " ").trim())
    .filter((s) => s.length > 1 && s.length < 200);
}

boards.forEach((board, i) => {
  const slug = `${i + 1}-${board.title.replace(/[^\w]+/g, "-")}`;
  const inner = unpack(board.html);
  console.log("\nBOARD", i + 1, board.title, "nested", inner ? inner.length : 0, "mime", board.mime);
  if (!inner) {
    const text = [...new Set(visibleText(board.html))];
    fs.writeFileSync(path.join(outDir, slug + ".txt"), text.join("\n"));
    console.log(" text lines", text.length);
    return;
  }
  inner.forEach((page, j) => {
    const deeper = unpack(page.html);
    const html = deeper ? deeper.map((d) => d.html).join("\n") : page.html;
    const text = [...new Set(visibleText(html))];
    const file = path.join(outDir, `${slug}--${j}-${(page.title || "page").toString().slice(0, 40).replace(/[^\w]+/g, "-")}.txt`);
    fs.writeFileSync(file, text.join("\n"));
    console.log("  page", j, page.title, page.mime, "chars", page.html.length, "text", text.length, "deeper", deeper ? deeper.length : 0);
    if (text.length < 30) {
      const strings = [...page.html.matchAll(/>([^<]{2,80})</g)].map((m) => m[1].trim()).filter(Boolean);
      console.log("    sample tags", [...new Set(strings)].slice(0, 40).join(" | "));
    }
  });
});
