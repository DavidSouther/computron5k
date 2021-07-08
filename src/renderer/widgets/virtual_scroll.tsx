import {
  Component,
  createRef,
  ReactElement,
  RefObject,
  UIEventHandler,
} from "react";
import { debounce } from "../../util/debounce";

interface VirtualScrollDataAdapter<T> {
  (offset: number, limit: number): Iterable<T>;
}

export function arrayAdapter<T>(data: T[]): VirtualScrollDataAdapter<T> {
  return (offset: number, limit: number) => data.slice(offset, offset + limit);
}

interface VirtualScrollProps<T> {
  settings: Partial<VirtualScrollSettings>;
  // Retrieve limit items starting at offset from whatever data source
  get: (offset: number, limit: number) => Iterable<T>;
  row: (t: T) => ReactElement;
}

interface VirtualScrollSettings {
  minIndex: number;
  maxIndex: number;
  startIndex: number;
  itemHeight: number; // px
  count: number; // The number of items to show
  tolerance: number; // The number of just-out-of-bounds items to hot swap
}

function fillVirtualScrollSettings(
  settings: Partial<VirtualScrollSettings>
): VirtualScrollSettings {
  const {
    minIndex = 0,
    maxIndex = 1,
    startIndex = 0,
    itemHeight = 20,
    count = maxIndex - minIndex + 1,
    tolerance = count,
  } = settings;

  return { minIndex, maxIndex, startIndex, itemHeight, count, tolerance };
}

interface VirtualScrollState<T> {
  settings: VirtualScrollSettings;
  bufferedItems: number; // Count
  totalHeight: number; // px
  viewportHeight: number; // px
  topPaddingHeight: number; // px
  bottomPaddingHeight: number; // px
  toleranceHeight: number; // px
  bufferHeight: number; // px
  initialPosition: number; // px
  data: T[];
}

class VirtualScroll<T> extends Component<VirtualScrollProps<T>> {
  state: VirtualScrollState<T>;
  viewportElement = createRef() as RefObject<HTMLDivElement>;

  constructor(props: VirtualScrollProps<T>) {
    super(props);

    const settings = fillVirtualScrollSettings(props.settings);
    this.state = this.initialState(settings);
  }

  render() {
    const {
      viewportHeight,
      topPaddingHeight,
      bottomPaddingHeight,
      data,
      settings: { itemHeight },
    } = this.state;

    return (
      <div
        style={{ height: viewportHeight, overflowY: "scroll" }}
        ref={this.viewportElement}
        onScroll={this.runScroller}
      >
        <div style={{ height: topPaddingHeight }}></div>
        {data.map((v, i) => (
          <div key={i} style={{ height: itemHeight }}>
            {this.props.row(v)}
          </div>
        ))}
        <div style={{ height: bottomPaddingHeight }}></div>
      </div>
    );
  }

  componentDidMount() {
    if (this.viewportElement.current) {
      this.viewportElement.current.scrollTop = this.state.initialPosition;
    }
    this.doScroll(this.viewportElement.current?.scrollTop ?? 0);
  }

  private doScroll(scrollTop: number) {
    const {
      totalHeight,
      toleranceHeight,
      bufferedItems,
      settings: { itemHeight, minIndex },
    } = this.state;
    const index =
      minIndex + Math.floor((scrollTop - toleranceHeight) / itemHeight);
    const data = this.getData(index, bufferedItems);
    const topPaddingHeight = Math.max((index - minIndex) * itemHeight, 0);
    const bottomPaddingHeight = Math.max(
      totalHeight - topPaddingHeight - data.length * itemHeight,
      0
    );

    this.setState({
      topPaddingHeight,
      bottomPaddingHeight,
      data,
    });
  }

  private getData(offset: number, limit: number): T[] {
    const { minIndex, maxIndex } = this.state.settings;
    const start = Math.max(0, minIndex, offset);
    const end = Math.min(maxIndex, offset + limit - 1);
    const data = this.props.get(start, end - start);
    return [...data];
  }

  private runScroller: UIEventHandler<HTMLDivElement> = debounce(
    ({ target }) => {
      const { scrollTop } = target as HTMLElement;
      this.doScroll(scrollTop);
    },
    0
  );

  private initialState(settings: VirtualScrollSettings): VirtualScrollState<T> {
    // From Denis Hilt, https://blog.logrocket.com/virtual-scrolling-core-principles-and-basic-implementation-in-react/
    const { minIndex, maxIndex, startIndex, itemHeight, count, tolerance } =
      settings;
    const bufferedItems = count + 2 * tolerance;
    const itemsAbove = startIndex - tolerance - minIndex;

    const viewportHeight = count * itemHeight;
    const totalHeight = (maxIndex - minIndex + 1) * itemHeight;
    const toleranceHeight = tolerance * itemHeight;
    const bufferHeight = viewportHeight + 2 * toleranceHeight;
    const topPaddingHeight = itemsAbove * itemHeight;
    const bottomPaddingHeight = totalHeight - topPaddingHeight;
    const initialPosition = topPaddingHeight + toleranceHeight;

    return {
      settings,
      viewportHeight,
      totalHeight,
      toleranceHeight,
      bufferHeight,
      bufferedItems,
      topPaddingHeight,
      bottomPaddingHeight,
      initialPosition,
      data: [],
    };
  }
}

export default VirtualScroll;
