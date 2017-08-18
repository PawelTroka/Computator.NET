export class SpecialSymbolsHandler {
    public /* const */ static DotSymbol: string = '·';

    public static replace(input: string, isExponent: boolean) : string {
        return input.replace("*", this.DotSymbol);
    }
}