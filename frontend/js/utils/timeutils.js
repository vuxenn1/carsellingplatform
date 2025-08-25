export function formatTimestamp(timestamp, detailed = true) {
  const date = new Date(timestamp);
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const hh = String(date.getHours()).padStart(2, '0');
  const min = String(date.getMinutes()).padStart(2, '0');
  const sec = String(date.getSeconds()).padStart(2, '0');
  const ms = String(date.getMilliseconds()).padStart(3, '0');

  if (detailed)
    return `${dd}.${mm}.${yyyy}, ${hh}:${min}:${sec}:${ms}`;
  else
    return `${dd}.${mm}.${yyyy}, ${hh}:${min}`;
}