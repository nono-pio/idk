﻿import CustomForm from "./CustomForm";
import MathInput from "../components/MathInput";
import { useState } from "react";
import Fetch from "../api/FetchAPI";
import MathExpr from "../components/MathExpr";
import {FormLabel} from "react-bootstrap";

export default function EvalForm({ setResults }) {
    async function onSubmit() {
        
        if (expression === "")
            return 
        
        let results = await Fetch("eval", {"expr": expression})
        if (results === null)
            setResults([ ])
        else
            setResults([
                {
                    domain: "Evaluation",
                    title: <>Evaluation of <MathExpr latex={expression} /></>,
                    content: <MathExpr latex={results.expr} />
                }
            ])
    }
    
    const [expression, setExpression] = useState("")
    
    return (
        <CustomForm title="Evaluation et Simplification" onSubmit={onSubmit}>
            <FormLabel>Enter the expression to evaluate and simplify</FormLabel>
            <MathInput className="mb-3" setLatex={setExpression} onEnter={onSubmit} />
        </CustomForm>
    )
}