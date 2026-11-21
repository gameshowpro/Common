// Date and Intl use the visitor's browser time zone, including its daylight-saving rules.
export function describe(utcInstants) {
    const zone = Intl.DateTimeFormat().resolvedOptions().timeZone || "browser local time";
    const offsetMinutes = utcInstants.map(instant => {
        const date = new Date(instant);
        if (Number.isNaN(date.getTime())) throw new Error("Invalid UTC instant");
        return -date.getTimezoneOffset();
    });
    return [zone, ...offsetMinutes.map(String)];
}
