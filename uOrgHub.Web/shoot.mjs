// Captures brochure screenshots of the HR & Payroll module from the running app.
import { chromium } from "playwright";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(__dirname, "../uOrgHub.HR/Resources/screenshots");
const WEB = "http://localhost:5173";

const pages = [
  ["hr",            "01-hr-dashboard"],
  ["hr/employees",  "02-employees"],
  ["hr/departments","03-departments"],
  ["hr/designations","04-designations"],
  ["hr/organogram", "05-organogram"],
  ["hr/leave",      "06-leave-management"],
  ["hr/payroll",    "07-payroll-grades"],
  ["hr/recruitment","08-recruitment"],
  ["hr/attendance", "09-attendance"],
  ["hr/performance","10-performance"],
];

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const run = async () => {
  const browser = await chromium.launch();
  const ctx = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
  });
  const page = await ctx.newPage();

  // --- Login page screenshot ---
  await page.goto(`${WEB}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector('input[placeholder="Enter username or email"]', { timeout: 20000 });
  await sleep(1200);
  await page.screenshot({ path: path.join(OUT, "00-login.png") });
  console.log("shot: 00-login");

  // --- Authenticate via the form ---
  await page.fill('input[placeholder="Enter username or email"]', "admin");
  await page.fill('input[placeholder="Enter password"]', "Admin@12345");
  await Promise.all([
    page.waitForURL((u) => !u.toString().includes("/login"), { timeout: 20000 }),
    page.click('button[type="submit"]'),
  ]);
  await sleep(1500);
  console.log("logged in, url:", page.url());

  // --- HR pages ---
  for (const [route, name] of pages) {
    try {
      await page.goto(`${WEB}/${route}`, { waitUntil: "networkidle", timeout: 20000 });
      await sleep(1800); // let react-query data + charts render
      await page.screenshot({ path: path.join(OUT, `${name}.png`) });
      console.log("shot:", name);
    } catch (e) {
      console.log("FAILED:", name, e.message);
    }
  }

  await browser.close();
  console.log("done ->", OUT);
};

run().catch((e) => { console.error(e); process.exit(1); });
