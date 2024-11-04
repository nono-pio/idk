export default async function Fetch(section, content) {
    const endpoint = `/api/${section}`
    const response = await fetch(endpoint, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(content)
    })

    if (!response.ok)
        return null
    
    const result = await response.json();

    return { endpoint, input:content, result }
}