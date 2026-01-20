(() => {
  function getTextArea() {
    return document.getElementById('myTextArea');
  }

  function wrapSelection(prefix, suffix) {
    const textarea = getTextArea();
    if (!textarea) return;

    const start = textarea.selectionStart ?? 0;
    const end = textarea.selectionEnd ?? 0;
    const value = textarea.value ?? '';
    const selected = value.substring(start, end);
    const updated = value.substring(0, start) + prefix + selected + suffix + value.substring(end);

    textarea.value = updated;
    const newStart = start + prefix.length;
    const newEnd = newStart + selected.length;
    textarea.setSelectionRange(newStart, newEnd);
    textarea.focus();
  }

  window.applyFormat = (kind) => {
    switch (kind) {
      case 'bold':
        wrapSelection('**', '**');
        break;
      case 'italic':
        wrapSelection('_', '_');
        break;
      case 'underline':
        wrapSelection('<u>', '</u>');
        break;
      default:
        break;
    }
  };
})();
