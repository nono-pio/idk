import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

export default function DerivativeForm({ setResults }) {
    async function onSubmit() {

        if (expression === "")
            return

        let fetch_results = await Fetch("derivative", {expr: expression, var: variable})
        const results = fetch_results.result
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Derivation",
                    title: <>Derivation of <MathExpr latex={expression} /></>,
                    content: <MathExpr latex={results.expr} />,
                    fetch: fetch_results 
                }
            ])
    }

    const [expression, setExpression] = useState("")
    const [variable, setVariable] = useState("x")
    
    return (
        <CustomForm title="Derivation" onSubmit={onSubmit}>
            <label>Enter the function to derive</label>
            <MathInput className="mb-3" setLatex={setExpression} />
            <label>Enter the variable</label>
            <MathInput className="mb-3" setLatex={setVariable} latex={variable} />
        </CustomForm>
    )
}