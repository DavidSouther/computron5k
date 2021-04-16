class Tree {
  private constructor(
    readonly token: string,
    readonly children: ReadonlyArray<Tree>
  ) {}
  static Leaf(token: string) {
    return Tree.Node(token, []);
  }
  static Node(token: string, children: ReadonlyArray<Tree>) {
    return new Tree(token, children);
  }
  toString(): string {
    return this.children.length > 0
      ? `(${this.token} ${this.children.map((t) => t.toString()).join(" ")})`
      : this.token;
  }
}

abstract class Operator {
  constructor(readonly token: string, readonly precedence: number) {}
  abstract leftAction(parser: Parser, left: Tree, token: string): Tree;
  abstract rightAction(parser: Parser, token: string): Tree;
}

class BinaryOperator extends Operator {
  leftAction(parser: Parser, left: Tree, token: string) {
    return Tree.Node(token, [left, parser.expression(this.precedence + 1)]);
  }
  rightAction(_: Parser, token: string) {
    return Tree.Leaf(`Err: Expected ${token} as infix operator`);
  }
}

const BinOp = (t: string, p: number) => new BinaryOperator(t, p);

class Parser {
  private current = 0;
  constructor(
    readonly operators: Map<string, Operator>,
    readonly tokens: ReadonlyArray<string>
  ) {}

  peek(): string {
    return this.tokens[this.current];
  }

  advance(): string {
    let peek = this.peek();
    this.current += 1;
    return peek;
  }

  expression(bp = 0): Tree {
    let token = this.advance();
    let left: Tree = this.right(token);
    while (bp < this.bp(this.peek())) {
      let token = this.advance();
      left = this.left(left, token);
    }
    return left;
  }

  private right(token: string) {
    return (
      this.operators.get(token)?.rightAction(this, token) ?? Tree.Leaf(token)
    );
  }

  private left(left: Tree, token: string) {
    return (
      this.operators.get(token)?.leftAction(this, left, token) ??
      Tree.Leaf(`Err: Expected infix operator, got ${token}`)
    );
  }

  private bp(token: string) {
    return this.operators.get(token)?.precedence ?? 0;
  }
}

class ParserFactory {
  private operators: Map<string, Operator>;
  constructor(operators: Iterable<Operator>) {
    this.operators = new Map([...operators].map((op) => [op.token, op]));
  }

  parse(input: string[]) {
    const parser = new Parser(this.operators, input);
    return parser.expression();
  }
}

const ETParser = new ParserFactory([BinOp("+", 10), BinOp("*", 20)]);
const program = "a + b * c + d".split(" ");
const tree = ETParser.parse(program);
const actual = tree.toString();
const expected = "(+ (+ a (* b c)) d)";
if (actual !== expected) {
  throw new Error(`Expected strings to be equal:\n\t${actual}\n\t${expected}`);
}
