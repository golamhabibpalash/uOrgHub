import { chromium } from "playwright";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(__dirname, "../uOrgHub.HR/Resources/screenshots");
const WEB = "http://localhost:5173";
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const b = await chromium.launch();
const ctx = await b.newContext({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 2 });
const p = await ctx.newPage();

await p.goto(`${WEB}/login`, { waitUntil: "networkidle" });
await p.waitForSelector('input[placeholder="Enter username or email"]', { timeout: 20000 });
await p.fill('input[placeholder="Enter username or email"]', "admin");
await p.fill('input[placeholder="Enter password"]', "Admin@12345");
await Promise.all([
  p.waitForURL((u) => !u.toString().includes("/login"), { timeout: 20000 }),
  p.click('button[type="submit"]'),
]);

await p.goto(`${WEB}/hr/employees`, { waitUntil: "networkidle" });
await sleep(1800);

// Click the first "View details" (eye) button in the Actions column.
await p.click('button[title="View details"]');
await sleep(1500); // wait for detail fetch + open animation
await p.screenshot({ path: path.join(OUT, "11-employee-details.png") });
console.log("shot: 11-employee-details");

await b.close();
console.log("done");
