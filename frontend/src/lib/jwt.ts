/** .NET JWT: ClaimTypes.Role = role claim URI + optional short `role` claim. */

const ROLE_CLAIM_URI = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split(".");
    if (parts.length < 2) return null;
    const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    const json = atob(padded);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function pushRoles(out: string[], value: unknown) {
  if (typeof value === "string") out.push(value);
  else if (Array.isArray(value))
    out.push(...value.filter((x): x is string => typeof x === "string"));
}

/** Operation claim names from JWT (örn. Admin, Cryo.Read). */
export function jwtRoles(token: string | null | undefined): string[] {
  if (!token) return [];
  const payload = decodeJwtPayload(token);
  if (!payload) return [];
  const roles: string[] = [];
  pushRoles(roles, payload[ROLE_CLAIM_URI]);
  pushRoles(roles, payload.role);
  return [...new Set(roles)];
}

export function hasJwtRole(token: string | null | undefined, role: string): boolean {
  return jwtRoles(token).includes(role);
}
