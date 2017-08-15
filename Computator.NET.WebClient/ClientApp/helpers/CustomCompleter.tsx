import * as Config from "../config";

export class CustomCompleter
{
    private autocompleteItems: any[];
    // private lang : any;

    public constructor(mode : string)
    {
        // this.lang = brace.acequire("lib/lang");

        const apiUrl = `${Config.WEB_API_BASE_URL}/autocomplete/${mode}`;
        console.log(`Calling ${apiUrl}`);

        fetch(apiUrl)
            .then(response => response.text() as Promise<string>)
            .then(data => {
                console.log(`Got result: count: ${data.length}`);
                this.autocompleteItems = (JSON.parse(data) as any[]);
                console.log(`Transformed result to array: count: ${this.autocompleteItems.length}`);
            });
    }

    private isNullOrWhiteSpace(str: string): boolean
    {
        return str == null || str.replace(/\s/g, "").length < 1;
    }

    public getCompletions(editor : any, session : any, pos : any, prefix: string, callback : any) : void
    {
        callback(null, (this.autocompleteItems).map(autocompleteItem =>
            ({
                title: autocompleteItem.details.title,
                description: autocompleteItem.details.description,
                caption: autocompleteItem.menuText,
                name: autocompleteItem.text,
                value: autocompleteItem.text,
                score: (autocompleteItem.text.indexOf(prefix) >= 0 ? 1 : 0) * (prefix.length / autocompleteItem.text.length),
                meta: this.isNullOrWhiteSpace(autocompleteItem.details.category) ? autocompleteItem.details.type : autocompleteItem.details.category
            })));
    }

    public getDocTooltip(item : any)
    {
        if (/*item.type == "snippet" &&*/ !item.docHTML) {
            item.docHTML = [
                "<b>", (item.title), "</b>", "<hr></hr>",
                (item.description)
            ].join("");
        }
    }
}