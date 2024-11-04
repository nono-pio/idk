import {Button, OverlayTrigger, Tooltip} from "react-bootstrap";

export default function ResultCard({ result }) {
    
    async function SignalError() {
        const response = await fetch("api/error", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(result.fetch)
        })
        
        if (!response.ok)
            console.log("A problem happen when reporting an math error")
    }
    
    return (
        <div className="card mb-3">
            <div className="card-header d-flex justify-content-between">
                <span>{result.domain}</span>
                <OverlayTrigger overlay={<Tooltip>Click the button if there is an error in the math</Tooltip>}>
                    <Button variant="outline-danger" size="sm" onClick={SignalError}>Math Error</Button>
                </OverlayTrigger>
            </div>
            <div className="card-body">
                <h5 className="card-title">{result.title}</h5>
                <p className="card-text">{result.content}</p>
            </div>
        </div>
    )
}