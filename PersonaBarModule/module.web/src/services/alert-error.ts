/** DNN ModelState validation error format */
interface IModelStateError {
  ModelState: Record<string, string[]>;
}

/** DNN API exception format */
interface IApiException {
  isApiException: true;
  response: string; // JSON string containing the actual error
}

/** Standard error with message */
interface IErrorWithMessage {
  Message: string;
}

/** Union type for all possible error formats */
type ErrorReason = string | IModelStateError | IApiException | IErrorWithMessage | Record<string, unknown>;

/**
 * Displays an alert dialog with appropriate error message based on the error format.
 * Handles various DNN error formats including ModelState validation errors and API exceptions.
 */
export default function alertError(reason: ErrorReason): void {
  // Handle string errors
  if (typeof reason === "string") {
    alert(reason);
    return;
  }

  // Handle null/undefined
  if (!reason || typeof reason !== "object") {
    alert("An unknown error occurred");
    return;
  }

  // Handle ModelState validation errors (ASP.NET format)
  if ("ModelState" in reason && reason.ModelState && typeof reason.ModelState === "object") {
    const modelState = (reason as IModelStateError).ModelState;
    const messages: string[] = [];

    for (const fieldKey in modelState) {
      if (Object.prototype.hasOwnProperty.call(modelState, fieldKey)) {
        const fieldErrors = modelState[fieldKey];
        if (Array.isArray(fieldErrors)) {
          messages.push(...fieldErrors);
        }
      }
    }

    if (messages.length > 0) {
      alert(messages.join("\n"));
      return;
    }
  }

  // Handle API exceptions with JSON response
  if ("isApiException" in reason && reason.isApiException && "response" in reason) {
    try {
      const apiException = reason as IApiException;
      const response = JSON.parse(apiException.response) as IErrorWithMessage;
      if (response && typeof response.Message === "string") {
        alert(response.Message);
        return;
      }
    } catch (parseError) {
      // If JSON parsing fails, fall through to other error handling
      console.warn("Failed to parse API exception response:", parseError);
    }
  }

  // Handle standard errors with Message property
  if ("Message" in reason && typeof (reason as IErrorWithMessage).Message === "string") {
    alert((reason as IErrorWithMessage).Message);
    return;
  }

  // Handle native Error objects or objects with string message property
  if (reason instanceof Error || (typeof reason === "object" && "message" in reason && typeof reason.message === "string")) {
    alert((reason as Error).message);
    return;
  }

  // Fallback: stringify the error object
  try {
    alert(JSON.stringify(reason, null, 2));
  } catch {
    alert("An error occurred that could not be displayed");
  }
}

/** Export the error types for use in other modules */
export type { ErrorReason, IModelStateError, IApiException, IErrorWithMessage };
