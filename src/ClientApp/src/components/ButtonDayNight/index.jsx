import React from "react";
import styles from "./styles.module.css";

export default function ButtonDayNight({ changeNight, isNight }) {
  return (
    <div className={styles.container}>
      <button className={styles.btn} type="button" onClick={() => {changeNight(isNight)}}>
        {isNight ? 'День' : 'Ночь'}
      </button>
    </div>
  )
}