import type { ExtensionAPI } from "@earendil-works/pi-coding-agent";
import { readFileSync, existsSync } from "node:fs";
import { join } from "node:path";

export default function (pi: ExtensionAPI) {
  pi.on("before_agent_start", async (event, ctx) => {
    const internalPath = join(ctx.cwd, "AGENTS_INTERNAL.md");

    if (!existsSync(internalPath)) return;

    const content = readFileSync(internalPath, "utf-8").trim();
    if (!content) return;

    return {
      systemPrompt:
        event.systemPrompt +
        `\n\n<context file="AGENTS_INTERNAL.md">\n${content}\n</context>`,
    };
  });
}
