// Scripted walkthrough for the README demo. Run: cd demo && npm run record
// The API reads demo/criteria.demo.json (a copy of src/Metis.Api/criteria.json made at start), so the live edit below
// never touches the committed criteria.
import { readFileSync, writeFileSync } from "node:fs";

const criteriaFile = new URL("./criteria.demo.json", import.meta.url);
const PASSWORD = "demo-only-passphrase"; // invented, set in demo.config.json

async function signIn(page) {
  await page.getByLabel("Email").fill("analyst@example.com");
  await page.getByLabel("Password").fill(PASSWORD);
  await page.getByRole("button", { name: "Sign in" }).click();
  await page.getByRole("table").waitFor();
}

export default async ({ page, step, shot }) => {
  await step("Metis: an origination analyst signs in (the token is held in memory, never in browser storage)", async () => {
    await page.getByLabel("Email").fill("analyst@example.com");
    await page.getByLabel("Password").pressSequentially(PASSWORD, { delay: 40 });
    await page.getByRole("button", { name: "Sign in" }).click();
    await page.getByRole("table").waitFor();
  });
  await step("Today's queue is ranked by criteria score, not arrival order (see the FCFS position column)");
  await shot("queue");
  await step("Every rank explains itself: the three criteria that moved its score the most", async () => {
    await page.getByRole("button", { name: "Rope Walk Student Housing" }).click();
  });
  await shot("reasons");
  await step("The weights belong to the head of credit and live in criteria.json", async () => {
    await page.getByRole("button", { name: "Weights in use" }).click();
  });
  await step("While the API keeps running, the head of credit raises Location from 10 to 40", async () => {
    const json = JSON.parse(readFileSync(criteriaFile, "utf8"));
    json.Scoring.Weights.Location = 40;
    writeFileSync(criteriaFile, JSON.stringify(json, null, 2));
    await page.waitForTimeout(1500);
  });
  await step("No restart, no deploy: Refresh re-ranks. Eastvale Logistics climbs to #2, the criteria version changes", async () => {
    await page.getByRole("button", { name: "Refresh" }).click();
    await page.getByRole("table").waitFor();
    await page.getByText("Location 40").waitFor(); // the open weights panel refreshes with the queue
    await page.getByRole("button", { name: "Eastvale Logistics Park" }).click();
    await page.waitForTimeout(1500);
  });
  await shot("reranked");
};

export async function mobile({ page, shot }) {
  await signIn(page);
  await shot("mobile");
}
