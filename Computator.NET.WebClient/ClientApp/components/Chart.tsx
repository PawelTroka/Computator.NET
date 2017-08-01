import * as Config from "../config";
import { ResizeHandler } from "../helpers/ResizeHandler"
import * as React from "react";
import { Expression } from "./Expression";
import {NumericInput} from "./NumericInput";
import "isomorphic-fetch";

interface IChartState
{
    xMin: number;
    xMax: number;
    yMin: number;
    yMax: number;
    expression : string;
}

export class Chart extends React.Component<{}, IChartState>
{
    public constructor() {
        super();
        this.state = { xMin: -5, xMax: 5, yMin: -5, yMax: 5, expression: "" };
        
        //this.drawChartClick.bind(this);
        //this.drawChart.bind(this);
        //this.redrawChart.bind(this);


        this.resizeHandler = new ResizeHandler();
        this.resizeHandler.afterResize = () => this.redrawChart();
    }

    public render(): JSX.Element {
        return <div>
            <h1>Chart</h1>
            <p>Write expression and draw chart</p>
            <br />
            <Expression expression={this.state.expression} onExpressionChange={v => this.setState(prevState => prevState.expression = v)} />

            <div ref={chartDivContainer => this.chartContainerDiv = chartDivContainer} className="col-md-10">
                <img ref={chartImage => this.chartImage = chartImage} src="" alt="chart" />
            </div>

            <div className="col-md-2">
                <br /><br />
                <button className="btn btn-primary btn-lg" onClick={e => this.drawChartClick(e)}>Draw chart!</button>
                <br /><br /><br />
                <h5>Chart area values:</h5>
                <ul className="list-group">
                    <li className="list-group-item">
                        <NumericInput label="X" subscriptLabel="MIN" value={-5} onValueChanged={v => this.setState(prevState => prevState.xMin = v)} />
                    </li>
                    <li className="list-group-item">
                        <NumericInput label="X" subscriptLabel="MAX" value={5} onValueChanged={v => this.setState(prevState => prevState.xMax = v)} />
                    </li>
                    <li className="list-group-item">
                        <NumericInput label="Y" subscriptLabel="MIN" value={-5} onValueChanged={v => this.setState(prevState => prevState.yMin = v)} />
                    </li>
                    <li className="list-group-item">
                        <NumericInput label="Y" subscriptLabel="MAX" value={5} onValueChanged={v => this.setState(prevState => prevState.yMax = v)} />
                    </li>
                </ul>
            </div>

        </div>;
    }

    private drawChartClick(event: React.MouseEvent<HTMLButtonElement>) : void
    {
        event.preventDefault();
        if (this.state.expression != null && this.state.expression !== "")
            this.drawChart(this.state.xMin, this.state.xMax, this.state.yMin, this.state.yMax, this.state.expression);
    }

    private drawChart(xmin: number, xmax: number, ymin: number, ymax: number, expression: string): void
    {

        const viewWidth = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
        const viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);

        const chartRect = this.chartContainerDiv.getBoundingClientRect();
        const height = Math.max(200, Math.round(viewHeight - chartRect.top));
        const width = Math.max(200, this.chartContainerDiv.clientWidth);

        const apiUrl = `${Config.WEB_API_BASE_URL}/chart/${width}/${height}/${xmin}/${xmax}/${ymin}/${ymax}/${encodeURIComponent(expression)}`;
        console.log(`Calling ${apiUrl}`);

        this.chartImage.src = apiUrl;
        this.chartImage.alt = expression;
    }

    private redrawChart(): void {
        if (this.state.expression != null && this.state.expression !== "")
            this.drawChart(this.state.xMin, this.state.xMax, this.state.xMin, this.state.yMax, this.state.expression);
    }

    private readonly resizeHandler: ResizeHandler;

    private chartImage: HTMLImageElement;
    private chartContainerDiv: HTMLDivElement;
}
