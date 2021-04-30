import React from "react";
import styles from "./style.module.css";

export default function ButtonRestart ({restart}) {
  return (
    <div className={styles.container}>
      <button
        className={styles.button}
        type="button"
        onClick={restart}
      >
        Рестарт
      </button>
    </div>
  )
}