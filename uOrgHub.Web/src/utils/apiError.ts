import { AxiosError } from "axios";

/**
 * Pulls the most specific message out of an API failure.
 *
 * Handles both shapes the API can return: ApiResponse<T> (`message` + `errors: string[]`)
 * and the framework's validation payload (`errors: { field: string[] }`). Falls back to the
 * axios message ("Request failed with status code 400") only when nothing better exists.
 */
export function extractApiError(err: unknown): string {
  const axiosErr = err as AxiosError<{
    message?: string;
    title?: string;
    errors?: string[] | Record<string, string[]>;
  }>;
  const body = axiosErr.response?.data;

  if (typeof body?.message === "string" && body.message) return body.message;

  if (body?.errors) {
    if (Array.isArray(body.errors)) {
      if (body.errors[0]) return body.errors[0];
    } else {
      const first = Object.values(body.errors).flat()[0];
      if (first) return first;
    }
  }

  if (typeof body?.title === "string" && body.title) return body.title;

  return (err as Error)?.message ?? "An error occurred";
}
