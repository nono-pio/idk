import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";

export default function EvalForm({ setResults }) {
    async function onSubmit() {
        
        if (expression === "")
            return 
        
        let result = await Fetch("eval", {"expr": expression})
        if (result === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Evaluation",
                    title: <>Evaluation of <MathExpr latex={expression} /></>,
                    content: <MathExpr latex={result.expr} />
                }
            ])
    }
    
    const [expression, setExpression] = useState("")
    
    return (
        <CustomForm title="Evaluation et Simplification of expressions" onSubmit={onSubmit}>
            <MathInput className="mb-3" setLatex={setExpression} />
        </CustomForm>
    )
}