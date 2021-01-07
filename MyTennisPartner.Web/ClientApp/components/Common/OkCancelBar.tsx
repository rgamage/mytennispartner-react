import * as React from 'react';

interface OkCancelProps {
    show: boolean;
    onOk?: (event: any) => void;
    onCancel?: (event: any) => void;
    addOffset: boolean;
}

export default function OkCancelBar(props: OkCancelProps) {

    if (!props.show) return null;
   
    return <nav className={`navbar navbar-light navbar-save fixed-top fixed-top-3 justify-content-end ${props.addOffset ? " top-offset" : ""}`}>
        <button className="btn btn-lg btn-primary" onClick={props.onOk}>Ok</button>
        <button className="btn btn-lg btn-secondary" onClick={props.onCancel}>Cancel</button>
    </nav>
}
