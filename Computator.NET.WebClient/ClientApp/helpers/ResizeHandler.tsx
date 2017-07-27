class ResizeHandler
{
    public constructor()
    {
        this.timeout = false;
        this.delta = 200;

        window.addEventListener("resize", () => this.handle());
    }

    private handle() : void
    {
        console.log("resize event");
        this.resizeTime = Date.now();
        if (this.timeout === false)
        {
            console.log("resize event timeout");
            this.timeout = true;
            setTimeout(() => this.resizeEnd(), this.delta);
        }
    }

    private resizeEnd(): void
    {
        const timePassed = Date.now() - this.resizeTime;
        console.log(`resize end after ${timePassed}ms`);
        if (timePassed < this.delta)
        {
            console.log("within threshold - will not fire afterResize event");
            setTimeout(() => this.resizeEnd(), this.delta);
        }
        else
        {
            console.log("after threshold - firing afterResize event");
            this.timeout = false;

            if(this.afterResize!=null)
                this.afterResize();

            console.log("Done resizing");
        }
    }

    public afterResize : () => void;

    private timeout: boolean;
    private resizeTime: number;

    private readonly delta : number;
}