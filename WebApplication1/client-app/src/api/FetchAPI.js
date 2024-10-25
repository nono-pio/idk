export default async function Fetch(section, content) {
    const response = await fetch(`/api/${section}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(content)
    })

    if (!response.ok)
        return null

    return await response.json()
}