import { StaticMathField } from "react-mathquill";

export default function MathExpr({ latex }) {
    return (
        <StaticMathField>{latex}</StaticMathField>
    )
}