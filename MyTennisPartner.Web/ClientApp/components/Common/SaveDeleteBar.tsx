import * as React from 'react';

interface SaveDeleteProps {
    show: boolean;
    showDelete: boolean;
    modalId?: string;
    onSave?: (event: any) => void;
    onDelete?: (event: any) => void;
    addOffset: boolean;
}

//class SaveDeleteStore {
//}

//const Store = new SaveDeleteStore();

// this makes a single instance of the store available to all components in the application
// this allows one component to set the properties for this component, even if it's in another part of the application
//export default Store;

export default function SaveDeleteBar(props: SaveDeleteProps) {

    if (!props.show) return null;

    // if mobile navbar is present, then we need a larger top offset for our menu
    //var mobileNavIsPresent = document.getElementById("navbar-mobile-menu") != null;

    let idString = `#${props.modalId}`;
    
    return <nav className={`navbar navbar-light navbar-save fixed-top fixed-top-3 justify-content-end ${props.addOffset ? " top-offset" : ""}`}>
        <button className="btn btn-lg btn-primary" onClick={props.onSave}>Save</button>
        <button hidden={!props.showDelete} className="btn btn-lg btn-danger" data-toggle="modal" data-target={idString}>Delete</button>
    </nav>
}
