export function validateEmail(email: string): boolean {
  if (!email) return false;

  const re = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

  return re.test(email.trim());
}

export function csvHasRequiredHeaders(headers: string[], requiredHeaders: string[]) {
  const normalizedHeaders = headers.map((h) => h.trim().toLowerCase());
  const issues: string[] = [];

  requiredHeaders.forEach((req) => {
    if (!normalizedHeaders.includes(req.trim().toLowerCase())) {
      issues.push(`${req} is missing from your headers.`);
    }
  });

  return issues;
}

export function splitCSVLine(line: string): string[] {
  const result: string[] = [];
  let current = "";
  let inQuotes = false;

  for (let i = 0; i < line.length; i++) {
    const char = line[i];

    if (char === '"' && line[i + 1] === '"') {
      current += '"';
      i++;
    } else if (char === '"') {
      inQuotes = !inQuotes;
    } else if (char === "," && !inQuotes) {
      result.push(current);
      current = "";
    } else {
      current += char;
    }
  }
  result.push(current);
  return result.map((s) => s.trim());
}
