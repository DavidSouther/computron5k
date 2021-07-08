import VirtualScroll, { arrayAdapter } from "./virtual_scroll";

describe("VirtualScroll", () => {
  it("tracks scroll position", () => {
    const data = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

    const scroll = new VirtualScroll({
      settings: { count: 3, startIndex: 2 },
      get: arrayAdapter(data),
      row: (i: number) => <div>{i}</div>,
    });
    scroll.componentDidMount();

    expect(scroll.state.bufferedItems).toBe(9);
    expect(scroll.state.topPaddingHeight).toBe(0);
    expect(scroll.state.viewportHeight).toBe(60);
    expect(scroll.state.totalHeight).toBe(200);
  });
});
