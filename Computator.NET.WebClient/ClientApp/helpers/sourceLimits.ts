export const maximumSourceCharacters = 65536;

/** Check both sources before either execution endpoint can allocate a worker. */
export function executionSourceError(expression: unknown, customFunctions: string): string {
  if (typeof expression === 'string' && expression.length > maximumSourceCharacters)
    return 'The expression or script exceeds 65,536 characters. Shorten it before running.';
  if (customFunctions.length > maximumSourceCharacters)
    return 'Custom functions exceed 65,536 characters. Shorten them in Custom functions before running.';
  return '';
}
