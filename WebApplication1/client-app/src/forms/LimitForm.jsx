import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

export default function LimitForm({ setResults }) {
    async function onSubmit() {

        if (expression === "" || to === "")
            return

        let fetch_results = await Fetch("limit", {expr: expression, var: variable, to:to})
        const results = fetch_results.result
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Limit",
                    title: <>Limit <MathExpr latex={`\\lim_{${variable} \\to ${to}} ${expression}`} /></>,
                    content: <MathExpr latex={results.expr + " = " + results.app} />,
                    fetch: fetch_results
                }
            ])
    }

    const [expression, setExpression] = useState("")
    const [variable, setVariable] = useState("x")
    const [to, setTo] = useState("")

    return (
        <CustomForm title="Limit" onSubmit={onSubmit}>
            <label>Enter the function</label>
            <MathInput className="mb-3" setLatex={setExpression} />
            <label>Enter the variable</label>
            <MathInput className="mb-3" setLatex={setVariable} latex={variable} />
            <label>Enter the limit</label>
            <MathInput className="mb-3" setLatex={setTo} />
        </CustomForm>
    )
}