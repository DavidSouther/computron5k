export function debounce(
  fn: (...args: any[]) => void,
  ms = 32
): (...args: unknown[]) => void {
  let timer = 0;
  return (...args: any[]) => {
    clearTimeout(timer);
    timer = setTimeout(
      () => (clearTimeout(timer), fn(...args)),
      ms
    ) as unknown as number; // Have to lie because node globals are annoying
  };
}
