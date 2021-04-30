import React from "react";
import styles from "./styles.module.css";

export default function Timer({ ticks }) {

  return (
    <div className={styles.container}>
      <div className={styles.timer}>Время: {ticks}</div>
    </div>
  )
}