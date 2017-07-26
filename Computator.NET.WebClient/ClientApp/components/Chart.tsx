import * as Config from '../config';
import * as React from 'react';
import "isomorphic-fetch";

interface IChartState
{
    xMin: number;
    xMax: number;
    yMin: number;
    yMax: number;
    expression: string;
}

export class Chart extends React.Component<{}, IChartState>
{

    public constructor()
    {
        super();
        this.state = { xMin: -5, xMax: 5, yMin: -5, yMax: 5, expression: "" };
        window.addEventListener("resize", () => this.redrawChart());
    }

    public render()
    {
        return <div>
                   <h1>Chart</h1>
                   <p>Write expression and draw chart</p>
                   <br/>
                   <div className="input-group input-group-lg">
                       <span className="input-group-addon" id="expression">Expression:</span>
                       <input type="text" defaultValue={this.state.expression} ref={expression => this.expressionInput=expression} className="form-control" placeholder="here write expression, example: 2x-cos(x)" aria-describedby="expression"/>
                   </div>

                   <div ref={chartDivContainer => this.chartContainerDiv = chartDivContainer} className="col-md-10">
                        <img ref={chartImage => this.chartImage=chartImage} src=""/>
                   </div>

                   <div className="col-md-2">
                       <br/><br/>
                       <button className="btn btn-primary btn-lg" onClick={e => this.drawChartClick(e)}>Draw chart!</button>
                       <br/><br/><br/>
                       <h5>Chart area values:</h5>
                       <ul className="list-group">
                           <li className="list-group-item">X<sub>MIN</sub> = <input defaultValue={this.state.xMin.toString()} ref={xmin => this.xMinInput = xmin} className="form-control" type="number" step="1"/></li>
                           <li className="list-group-item">X<sub>MAX</sub> = <input defaultValue={this.state.xMax.toString()} ref={xmax => this.xMaxInput = xmax} className="form-control" type="number" step="1"/></li>
                           <li className="list-group-item">Y<sub>MIN</sub> = <input defaultValue={this.state.yMin.toString()} ref={ymin => this.yMinInput = ymin} className="form-control" type="number" step="1"/></li>
                           <li className="list-group-item">Y<sub>MAX</sub> = <input defaultValue={this.state.yMax.toString()} ref={ymax => this.yMaxInput = ymax} className="form-control" type="number" step="1"/></li>
                       </ul>
                   </div>

               </div>;
    }

    private drawChartClick(event : any) : void
    {
        event.preventDefault();
        this.drawChart(Number(this.xMinInput.value),Number(this.xMaxInput.value),Number(this.yMinInput.value),Number(this.yMaxInput.value),this.expressionInput.value );
    }

    private drawChart(xmin: number, xmax: number, ymin: number, ymax: number, expression: string) : void {
        
        const viewWidth = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
        const viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);

        const chartRect = this.chartContainerDiv.getBoundingClientRect();
        const height = Math.max(200, Math.round(viewHeight - chartRect.top));
        const width = Math.max(200, this.chartContainerDiv.clientWidth);

        const apiUrl = `${Config.WEB_API_BASE_URL}/chart/${width}/${height}/${xmin}/${xmax}/${ymin}/${ymax}/${encodeURIComponent(expression)}`;
        console.log(`Calling ${apiUrl}`);
        this.chartImage.src = apiUrl;

        this.setState({xMin: xmin, xMax: xmax, yMin: ymin, yMax: ymax, expression: expression});
    }

    private redrawChart(): void
    {
        if (this.state.expression!=null && this.state.expression !== "")
            this.drawChart(this.state.xMin,this.state.xMax,this.state.xMin,this.state.yMax,this.state.expression);
    }

    private xMinInput : HTMLDataElement;
    private xMaxInput : HTMLDataElement;
    private yMinInput : HTMLDataElement;
    private yMaxInput : HTMLDataElement;
    private expressionInput : HTMLDataElement;
    private chartImage : HTMLImageElement;
    private chartContainerDiv : HTMLDivElement;
}
