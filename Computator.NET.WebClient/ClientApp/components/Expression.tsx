import * as Config from "../config";
import * as React from "react";
import * as Awesomplete from "awesomplete";
import "isomorphic-fetch";
import * as Autosuggest from "react-autosuggest";

interface IExpressionProps {
    expression: string;
    onExpressionChange: (expr: string) => void;
}

const languages = [
    {
        name: 'C',
        year: 1972
    },
    {
        name: 'Elm',
        year: 2012
    }
];

const getSuggestions = value => {
    const inputValue = value.trim().toLowerCase();
    const inputLength = inputValue.length;

    return inputLength === 0 ? [] : languages.filter(lang =>
        lang.name.toLowerCase().slice(0, inputLength) === inputValue
    );
};

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.name;

// Use your imagination to render suggestions.
const renderSuggestion = suggestion => (
    <div>
        {suggestion.name}
    </div>
);


export class Expression extends React.Component<IExpressionProps, {}>
{
    public constructor(props: IExpressionProps) {
        super(props);
        //this.handleChange = this.handleChange.bind(this);
    }

    public componentDidMount(): void {
        //var expressionInput = document.getElementById("expressionInput") as HTMLInputElement;

        //const awesompleteOptions =
        //    {
        //        list: ["Ada", "Java", "JavaScript", "Brainfuck", "LOLCODE", "Node.js", "Ruby on Rails"],
        //        filter: function (text, input) {
        //            return Awesomplete.FILTER_CONTAINS(text, input.match(/[^,]*$/)[0]);
        //        },

        //        item: function (text, input) {
        //            return Awesomplete.ITEM(text, input.match(/[^,]*$/)[0]);
        //        },

        //        replace: function (text) {
        //            var before = expressionInput.value.match(/^.+,\s*|/)[0];
        //            expressionInput.value = before + text + ", ";
        //        }
        //    } as Awesomplete.Options;

        //const awesomplete = new Awesomplete(expressionInput, awesompleteOptions);
    }

    // Autosuggest will call this function every time you need to update suggestions.
    // You already implemented this logic above, so just use it.
    onSuggestionsFetchRequested = ({ value }) => {

    };

    // Autosuggest will call this function every time you need to clear suggestions.
    onSuggestionsClearRequested = () => {

    };

    public render(): JSX.Element
    {
        const suggestions = languages;

        return <Autosuggest
            renderInputComponent={inputProps =>
                <div className="input-group input-group-lg">
                    <span className="input-group-addon"><span className="glyphicon glyphicon-chevron-right" aria-hidden="true"></span> Expression:</span>
                    <input {...inputProps} id="expressionInput" type="text" className="form-control" placeholder="here write expression, example: 2x-cos(x)" aria-describedby="expression" required />
                </div>
            }
            suggestions={suggestions}
            onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
            onSuggestionsClearRequested={this.onSuggestionsClearRequested}
            getSuggestionValue={getSuggestionValue}
            renderSuggestion={renderSuggestion}
            inputProps={{
                value: this.props.expression,
                onChange: e => this.handleChange(e)
            }}
        />;
    }

    private handleChange(event: React.ChangeEvent<HTMLInputElement>): void {
        const newValue = event.target.value;
        if (newValue != null) {
            console.log(`Changing expression to ${newValue}`);
            if (this.props.onExpressionChange != null)
                this.props.onExpressionChange(newValue);
        }

    }
}